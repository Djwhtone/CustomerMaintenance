using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CustomerMaintenance.Model.DataLayer;

namespace CustomerMaintenance
{
    public partial class frmCustomerMaintenance : Form
    {
        public frmCustomerMaintenance()
        {
            InitializeComponent();
        }

        private MMABooksContext context = new MMABooksContext();
        private Customers selectedCustomer;

        // private constants for the index values of the Modify and Delete button columns
        private const int modIndex = 6;
        private const int deleteIndex = 7;

        private const int maxRows = 10;
        private int totRows = 0;
        private int page = 0;
        
        private int numPages = 1;


        private void frmCustomerMaintenance_Load(object sender, EventArgs e)
        {
            totRows = context.Customers.Count();
            page = totRows / maxRows;

            if (totRows % maxRows != 0)
            {
                page += 1;
            }
            numPages = 1;

            DisplayCustomers();
        }

        private void DisplayCustomers()
        {
            dgvCustomers.Columns.Clear(); // Clear columns to prevent creating the mods/del buttons

            int skip = maxRows * (numPages - 1);

            int take = maxRows;
            if (numPages == page)
            {
                take = totRows - skip;
            }
            if (totRows <= maxRows)
            {
                take = totRows;
            }

            // get customers and bind grid
            var customers = context.Customers.OrderBy(c => c.Name)
                .Select(c => new { c.CustomerId, c.Name, c.Address, c.City, c.State, c.ZipCode }).Skip(skip).Take(take).ToList();
            dgvCustomers.DataSource = customers;
            // format grid

            dgvCustomers.Columns[0].Visible = false;
            
            
            dgvCustomers.EnableHeadersVisualStyles = false;
            dgvCustomers.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);

            dgvCustomers.ColumnHeadersDefaultCellStyle.BackColor = Color.Goldenrod;
            dgvCustomers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCustomers.AlternatingRowsDefaultCellStyle.BackColor = Color.PaleGoldenrod;

            dgvCustomers.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            //First column format

            //dgvCustomers.Columns[1].HeaderText = "Name";
            dgvCustomers.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCustomers.Columns[1].Width = 120;

            dgvCustomers.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCustomers.Columns[2].Width = 150;

            dgvCustomers.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCustomers.Columns[3].Width = 100;

            dgvCustomers.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCustomers.Columns[4].HeaderText = "ST";   
            dgvCustomers.Columns[4].Width = 30;

            dgvCustomers.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCustomers.Columns[5].Width = 90;

            var modifyButtomColumn = new DataGridViewButtonColumn()
            {
                UseColumnTextForButtonValue = true,
                HeaderText = "Modify Customer",
                Text = "Modify",
                Name = "modColumn"
            };
            dgvCustomers.Columns.Insert(modIndex, modifyButtomColumn);

            var deleteButtomColumn = new DataGridViewButtonColumn()
            {
                UseColumnTextForButtonValue = true,
                HeaderText = "Delete Customer",
                Text = "Delete",
                Name = "modDelete"
            };
            dgvCustomers.Columns.Insert(deleteIndex, deleteButtomColumn);

            EnableDisableButtons();

        }

        private void EnableDisableButtons()
        {
            if (numPages == 1)
            {
                btnFirstPage.Enabled = false;
                btnPrevious.Enabled = false;

            }
            else
            {
                btnFirstPage.Enabled = true;
                btnPrevious.Enabled = true;

            }
            if (numPages == page)
            {
                btnNext.Enabled = false;
                btnNext.Enabled = false;

            }
            else
            {
                btnNext.Enabled = true;
                btnNext.Enabled = true;

            }
        }

        private void ModifyCustomer()
        {
            var addModifyCustomerForm = new frmAddModifyCustomer()
            {
                AddCustomer = false,
                Customer = selectedCustomer,
                States = context.States.OrderBy(s => s.StateName).ToList()
            };

            DialogResult result = addModifyCustomerForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    selectedCustomer = addModifyCustomerForm.Customer;
                    context.SaveChanges();
                    DisplayCustomers();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    HandleConcurrencyError(ex);
                }
                catch (DbUpdateException ex)
                {
                    HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    HandleGeneralError(ex);
                }
            }
        }

        private void DeleteCustomer()
        {
            DialogResult result =
                MessageBox.Show($"Delete {selectedCustomer.Name}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    context.Customers.Remove(selectedCustomer);
                    context.SaveChanges();
                    DisplayCustomers();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    HandleConcurrencyError(ex);
                }
                catch (DbUpdateException ex)
                {
                    HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    HandleGeneralError(ex);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var addModifyCustomerForm = new frmAddModifyCustomer()
            {
                AddCustomer = true,
                States = context.States.OrderBy(s => s.StateName).ToList()
            };
            DialogResult result = addModifyCustomerForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    selectedCustomer = addModifyCustomerForm.Customer;
                    context.Customers.Add(selectedCustomer);
                    context.SaveChanges();
                    DisplayCustomers();
                }
                catch (DbUpdateException ex)
                {
                    HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    HandleGeneralError(ex);
                }
            }
        }

        private void HandleConcurrencyError(DbUpdateConcurrencyException ex)
        {
            ex.Entries.Single().Reload();

            var state = context.Entry(selectedCustomer).State;
            if (state == EntityState.Detached)
            {
                MessageBox.Show("Another user has deleted that product.",
                    "Concurrency Error");
            }
            else
            {
                string message = "Another user has updated that product.\n" +
                    "The current database values will be displayed.";
                MessageBox.Show(message, "Concurrency Error");
            }
            this.DisplayCustomers();
        }

        private void HandleDatabaseError(DbUpdateException ex)
        {
            string errorMessage = "";
            var sqlException = (SqlException)ex.InnerException;
            foreach (SqlError error in sqlException.Errors)
            {
                errorMessage += "ERROR CODE:  " + error.Number + " " +
                                error.Message + "\n";
            }
            MessageBox.Show(errorMessage);
        }

        private void HandleGeneralError(Exception ex)
        {
            MessageBox.Show(ex.Message, ex.GetType().ToString());
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvCustomers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int customerId = Convert.ToInt32(dgvCustomers.Rows[e.RowIndex].Cells[0].Value.ToString().Trim());
                selectedCustomer = context.Customers.Find(customerId);

                if(e.ColumnIndex == modIndex)
                {
                    ModifyCustomer();

                }
                if(e.ColumnIndex == deleteIndex)
                {
                    DeleteCustomer();
                }
            }
        }

        private void btnFirstPage_Click(object sender, EventArgs e)
        {
            numPages = 1;
            DisplayCustomers();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            numPages -= 1;
            DisplayCustomers();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            numPages += 1;
            DisplayCustomers();
        }

        private void btnLastPage_Click(object sender, EventArgs e)
        {
            numPages = page;
            DisplayCustomers();

        }
    }
}
